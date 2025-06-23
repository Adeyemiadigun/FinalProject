using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Entitties
{
    public class Option
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public ICollection<SelectedOption> SelectedOptions { get; set; } = [];


    }
}
