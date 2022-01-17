using System.ComponentModel.DataAnnotations;
using WebAPIAutores.Validaciones;

namespace WebAPIAutores.Entidades
{
    public class Autor
    {
        public int Id { get; set; }

        [PrimeraLetraMayuscula]
        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(maximumLength: 120, ErrorMessage = "El campo {0} no debe tener más de {1} caracteres")]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }
    }
}
