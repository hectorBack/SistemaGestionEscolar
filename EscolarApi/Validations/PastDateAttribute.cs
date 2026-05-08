using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.Validations
{
    public class PastDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime > DateTime.Now)
                {
                    return new ValidationResult("La fecha de contratación no puede ser una fecha futura.");
                }

                // Opcional: Validar que no sea una fecha de hace 100 años
                if (dateTime < DateTime.Now.AddYears(-60))
                {
                    return new ValidationResult("La fecha de contratación es demasiado antigua.");
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Fecha no válida.");
        }

    }
}