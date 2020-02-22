using System;
using System.Collections.Generic;
using System.Text;

namespace MockMyDbTests
{
    public class Student
    {
        public Guid StudentId { get; set; }
        public string Name { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }
}
