using AutoMapper;
using Elfie.Serialization;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;

namespace Gauniv.WebServer.Dtos
{
    public class GameDto
    {
        public int Id { get; set; }
        public String? Name { get; set; }
        public String? Description { get; set; }
        public BinaryData? Payload { get; set; }
        public float Price { get; set; }
        public List<String> Categories { get; set; } = new List<String>();
    }
}