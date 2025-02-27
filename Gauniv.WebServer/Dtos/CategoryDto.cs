using AutoMapper;
using Elfie.Serialization;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;

namespace Gauniv.WebServer.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public List<String> Games { get; set; } = new List<String>();
    }
}