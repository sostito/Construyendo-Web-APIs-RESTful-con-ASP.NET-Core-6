using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{id}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroDTO>> Get(int id)
        {
            var libro = await context.Libros.
                Include(libroDB => libroDB.AutoresLibros).
                ThenInclude(autorLibroDB => autorLibroDB.Autor).
                FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
                return NotFound();

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();
            return mapper.Map<LibroDTO>(libro);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] LibroCreacionDTO libroCreacionDTO)
        {
            if (libroCreacionDTO.AutoresIds == null)
                return BadRequest("No se puede crear un libro sin autores");

            var autoresIds = await context.Autores
                .Where(autorBD => libroCreacionDTO.AutoresIds.Contains(autorBD.Id)).Select(x => x.Id).ToListAsync();

            if (libroCreacionDTO.AutoresIds.Count != autoresIds.Count)
            {
                return BadRequest("No existe uno de los autores enviados");
            }

            var libro = mapper.Map<Libro>(libroCreacionDTO);
            AsignarOrdenAutore(libro);

            context.Add(libro);
            await context.SaveChangesAsync();

            //var libroDTO = mapper.Map<LibroDTO>(libro);

            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libro);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put([FromBody] LibroCreacionDTO libroCreacionDTO, [FromRoute] int id)
        {
            var libroBD = await context.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroBD == null)
                return NotFound();

            libroBD = mapper.Map(libroCreacionDTO, libroBD);
            AsignarOrdenAutore(libroBD);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch([FromBody] JsonPatchDocument<LibroPatchDTO> patchDocument, [FromRoute] int id)
        {
            if (patchDocument == null)
                return BadRequest();

            var libroBD = await context.Libros
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroBD == null)
                return NotFound();


            var libroDTO = mapper.Map<LibroPatchDTO>(libroBD);
            patchDocument.ApplyTo(libroDTO, ModelState);

            var esValido = TryValidateModel(libroDTO);

            if (!esValido)
                return BadRequest(ModelState);

            mapper.Map(libroDTO, libroBD); // es lo mismo que usar mapper.Map<destino>(inicial);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var existe = await context.Libros.AnyAsync(x => x.Id == id);

            if (!existe)
                return NotFound();

            context.Remove(new Libro() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
        private void AsignarOrdenAutore(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }
    }
}
