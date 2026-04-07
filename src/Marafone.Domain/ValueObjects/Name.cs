using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marafone.Domain.ValueObjects
{
    public record Name
    {
        public string name { get; init; }
        public Name(string n)
        {
            if (!string.IsNullOrWhiteSpace(n) && n.Length <= 16)
                name = n;
            else
                throw new ArgumentException("nome non valido");
        }
    }
}
