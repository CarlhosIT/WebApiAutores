using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;
using WebApiAutores.Filtros;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    public class AutoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly ServiceScoped sscoped;
        private readonly ServiceSingletone ssingletone;
        private readonly ServiceTransient stransient;
        private readonly IMapper mapper;

        public AutoresController(ApplicationDbContext context, ServiceScoped Sscoped, ServiceSingletone Ssingletone, ServiceTransient Stransient, IMapper mapper)
        {
            this.context = context;
            sscoped = Sscoped;
            ssingletone = Ssingletone;
            stransient = Stransient;
            this.mapper = mapper;
        }



        [HttpGet]
        [ServiceFilter(typeof(MyFiltro))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<AutorDTO>> Get()
        {
            var autores = await context.Autores.ToListAsync();
            return mapper.Map<List<AutorDTO>>(autores);

        }



        [HttpGet("guid")]
        [ResponseCache(Duration = 10)]
        [ServiceFilter(typeof(MyFiltro))]
        public ActionResult<List<Autor>> GetGuid()
        {
            return Ok(new { AutoresControllerScoped = sscoped.guid, AutoresControllerTrasient = stransient.guid,AutoresControllerSIngleton=ssingletone.guid  });

        }



        [HttpGet("{id:int}", Name ="obtenerAutor")]
        public async Task<ActionResult<AutorDTOCOnLIbros>> Get(int id) 
        {
            var autor = await context.Autores.Include(autorBD=>autorBD.autoresLibros)
                .ThenInclude(autorBD=>autorBD.libro)
                .FirstOrDefaultAsync(AutorBD=> AutorBD.Id==id);
            if (autor == null) 
            {
                return NotFound();
            }
            return mapper.Map<AutorDTOCOnLIbros>(autor);
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDTO>>> Get([FromRoute]String nombre)
        {
            var autores = await context.Autores.Where(AutorBD => AutorBD.Nombre.Contains(nombre)).ToListAsync();
            if (autores == null)
            {
                return NotFound();
            }
            return mapper.Map<List<AutorDTO>>(autores);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody]AutorCreacionDTO autorCreacion)
        {
            var ExisteAutorConMismoNombre = await context.Autores.AnyAsync(AutorBD=>AutorBD.Nombre== autorCreacion.Nombre);
            if (ExisteAutorConMismoNombre) 
            {
                return BadRequest($"Ya existe el autor con el mismo nombre {autorCreacion.Nombre}");
            }

            var autor = mapper.Map<Autor>(autorCreacion);
            context.Add(autor);
            await context.SaveChangesAsync();
            var autorDTO = mapper.Map<AutorDTO>(autor);
            return CreatedAtRoute("obtenerAutor", new { id=autor.Id},autorDTO);
        }



        
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(AutorCreacionDTO autorDTO, int id) 
        {
           
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            var autor = mapper.Map<Autor>(autorDTO);
            context.Update(autor);
            await context.SaveChangesAsync();
            return NoContent();

        }
        
        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,Policy ="EsAdmin")]
        public async Task<ActionResult> Delete(int id) 
        {
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe) 
            {
                return NotFound();
            }
            context.Remove(new Autor() {Id=id });
            await context.SaveChangesAsync();
            return Ok();
        }

        
    }
}
