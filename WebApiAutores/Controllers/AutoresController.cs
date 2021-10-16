using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiAutores.Entidades;
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

        public AutoresController(ApplicationDbContext context,ServiceScoped Sscoped,ServiceSingletone Ssingletone,ServiceTransient Stransient)
        {
            this.context = context;
            sscoped = Sscoped;
            ssingletone = Ssingletone;
            stransient = Stransient;
        }
        [HttpGet]
        public async Task<ActionResult<List<Autor>>> Get()
        {
            return await context.Autores.ToListAsync();

        }
        [HttpGet("guid")]
        public ActionResult<List<Autor>> GetGuid()
        {
            return Ok(new { AutoresControllerScoped = sscoped.guid, AutoresControllerTrasient = stransient.guid,AutoresControllerSIngleton=ssingletone.guid  });

        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Autor>> Get(int id) 
        {
            var autor = await context.Autores.FirstOrDefaultAsync(x=> x.Id==id);
            if (autor == null) 
            {
                return NotFound();
            }
            return autor;
        }
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]Autor autor)
        {
            var ExisteAutorConMismoNombre = await context.Autores.AnyAsync(x=>x.Nombre==autor.Nombre);
            if (ExisteAutorConMismoNombre) 
            {
                return BadRequest($"Ya existe el autor con el mismo nombre {autor.Nombre}");
            }
            context.Add(autor);
            await context.SaveChangesAsync();
            return Ok();
        }


        
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(Autor autor, int id) 
        {
            if (autor.Id != id) 
            {
                return BadRequest("El id del autor no corresponde al URL");
            }
            var existe = await context.Autores.AnyAsync(x => x.Id == id);
            if (!existe)
            {
                return NotFound();
            }
            context.Update(autor);
            await context.SaveChangesAsync();
            return Ok();

        }
        
        [HttpDelete("{id:int}")]
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
