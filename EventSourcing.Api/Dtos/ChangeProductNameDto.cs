using System;

namespace EventSourcing.Api.Dtos
{
    public class ChangeProductNameDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}