using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarApi.Validations
{
    public class MinMaxAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public MinMaxAgeAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - fechaNacimiento.Year;
                if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

                if (edad < _minAge)
                    return new ValidationResult($"El alumno debe tener al menos {_minAge} años.");

                if (edad > _maxAge)
                    return new ValidationResult($"La edad no puede ser mayor a {_maxAge} años.");

                if (fechaNacimiento > hoy)
                    return new ValidationResult("La fecha de nacimiento no puede ser una fecha futura.");

                return ValidationResult.Success;
            }

            return new ValidationResult("Fecha de nacimiento no válida.");
        }
    }
}