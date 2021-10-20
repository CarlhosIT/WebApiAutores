using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/cuentas")]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly IDataProtector dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration, 
            SignInManager<IdentityUser> SignInManager, IDataProtectionProvider dataProtectionProvider, HashService hashService)
        {
            this.userManager = userManager;
            Configuration = configuration;
            signInManager = SignInManager;
            HashService = hashService;
            dataProtector = dataProtectionProvider.CreateProtector("valor_Secreto_");
        }

        public IConfiguration Configuration { get; }
        public HashService HashService { get; }

        [HttpGet("hash/{Texto}")]
        public ActionResult Hash([FromRoute]String Texto) 
        {
            var resultado1 = HashService.Hash(Texto);
            var resultda2 = HashService.Hash(Texto);

            return Ok(
                        new
                        {
                            TextoPlano=Texto,
                            Hash1=resultado1,
                            Hash2=resultda2
                        }
                );
        }


        [HttpGet("encriptar")]
        public ActionResult Encripatar() 
        {
            var textoPlano = "Carlhos Edgardo";
            var TextoCifrado = dataProtector.Protect(textoPlano);
            var TextoDesencriptado = dataProtector.Unprotect(TextoCifrado);

            return Ok(
                        new 
                        {
                            TextoPlano=textoPlano,
                            TextoCifrado=TextoCifrado,
                            Textodesencriptado=TextoDesencriptado
                        }
                     );
        }


        [HttpPost("registrar")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credenciales)
        {
            var usuario = new IdentityUser { UserName = credenciales.Emali, Email = credenciales.Emali, };
            var resultado = await userManager.CreateAsync(usuario, credenciales.password);

            if (resultado.Succeeded)
            {
                return await ConstruirTOken(credenciales);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuarioDTO credenciales)
        {
            var resultado = await signInManager.PasswordSignInAsync(credenciales.Emali, credenciales.password,
            isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await ConstruirTOken(credenciales);
            }
            else
            {
                return BadRequest("Error en usuario y contraseña");

            }

        }
        [HttpGet("renovartoken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuarioDTO() { Emali = email };
            return await ConstruirTOken(credencialesUsuario);
        }

        [HttpPost("HacerAdmin")]
        public async Task<ActionResult> HacerAdmin(HacerAdminDTO hacerAdminDTO) 
        {
            var usuario = await userManager.FindByEmailAsync(hacerAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin","1"));
            return NoContent();
        }

        [HttpPost("RemoverAdmin")]
        public async Task<ActionResult> RemoverAdmin(HacerAdminDTO quitarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(quitarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }


        private async Task<RespuestaAutenticacionDTO> ConstruirTOken(CredencialesUsuarioDTO credencial) 
        {
            var claims = new List<Claim>()
            {
                new Claim("email",credencial.Emali),
                new Claim("Valor","Otro valor")
            };
            var usuario = await userManager.FindByEmailAsync(credencial.Emali);
            var claimBD = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimBD);

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Llavejwt"]));
            var creds = new SigningCredentials(llave,SecurityAlgorithms.HmacSha256);
            var expiracion = DateTime.UtcNow.AddMinutes(15);

            var SecurityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,expires:expiracion,signingCredentials:creds);
            return new RespuestaAutenticacionDTO() { Token = new JwtSecurityTokenHandler().WriteToken(SecurityToken),Expiracion=expiracion };
        }

    }
}
