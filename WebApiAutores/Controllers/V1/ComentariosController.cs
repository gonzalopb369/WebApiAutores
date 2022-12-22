using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.DTOs;
using WebApiAutores.Entidades;

namespace WebApiAutores.Controllers.V1
{
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    [ApiController]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext contexto;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext contexto,
                        IMapper mapper,
                        UserManager<IdentityUser> userManager)
        {
            this.contexto = contexto;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        [HttpGet(Name = "obtenerComentariosLibro")]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await contexto.Libros.AnyAsync(libroDB => libroDB.Id == libroId);
            if (!existeLibro)
                return NotFound();
            var comentarios = await contexto.Comentarios
                        .Where(comentarioDB => comentarioDB.LibroId == libroId).ToListAsync();
            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }


        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            //var comentario = await contexto.Comentarios
            //                .Where(comDB => comDB.Id == id).FirstOrDefaultAsync();
            var comentario = await contexto.Comentarios.FirstOrDefaultAsync(comDB => comDB.Id == id);
            if (comentario == null)
                return NotFound();
            return mapper.Map<ComentarioDTO>(comentario);
        }


        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;
            var existeLibro = await contexto.Libros.AnyAsync(libroDB => libroDB.Id == libroId);
            if (!existeLibro)
                return NotFound();
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuarioId;
            contexto.Add(comentario);
            await contexto.SaveChangesAsync();
            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("obtenerComentario",
                    new { id = comentario.Id, libroId }, comentario);
        }


        [HttpPut("{id:int}", Name = "actualizarComentario")]
        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var existeLibro = await contexto.Libros.AnyAsync(libroDB => libroDB.Id == libroId);
            if (!existeLibro)
                return NotFound();
            var existeComentario = await contexto.Comentarios.AnyAsync(comenDB => comenDB.Id == id);
            if (!existeComentario)
                return NotFound();
            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.Id = id;
            contexto.Update(comentario);
            await contexto.SaveChangesAsync();
            return NoContent();
        }
    }
}
