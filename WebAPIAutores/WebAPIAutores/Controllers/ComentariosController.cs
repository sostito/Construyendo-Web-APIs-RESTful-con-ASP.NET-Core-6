using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId}/comentarios")]
    public class ComentariosController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly UserManager<IdentityUser> userManager;

        public ComentariosController(ApplicationDbContext context, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(libroDb => libroDb.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentarios = await context.Comentarios.Where(comentarioDb => comentarioDb.LibroId == libroId).ToListAsync();
            return mapper.Map<List<ComentarioDTO>>(comentarios);
        }

        [HttpGet("{id}", Name = "ObtenerComentario")]
        public async Task<ActionResult<ComentarioDTO>> GetPorId(int id)
        {
            var comentario = await context.Comentarios.FirstAsync(libroDb => libroDb.Id == id);

            if (comentario == null)
                return NotFound();

            return mapper.Map<ComentarioDTO>(comentario);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDTO comentarioCreacionDTO)
        {
            var email = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault().Value;
            var usuario = await userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;
            var existeLibro = await context.Libros.AnyAsync(libroDb => libroDb.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.LibroId = libroId;
            comentario.UsuarioId = usuarioId;
            context.Add(comentario);
            await context.SaveChangesAsync();

            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return Ok(comentarioDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromBody] ComentarioCreacionDTO comentarioCreacionDTO, [FromRoute] int id, int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(libroDb => libroDb.Id == libroId);

            if (!existeLibro)
                return NotFound();

            var existeComentario = await context.Comentarios.AnyAsync(comentarioDB => comentarioDB.Id == id);

            if (!existeComentario)
                return NotFound();

            var comentario = mapper.Map<Comentario>(comentarioCreacionDTO);
            comentario.Id = id;
            comentario.LibroId = libroId;
            context.Update(comentario);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
