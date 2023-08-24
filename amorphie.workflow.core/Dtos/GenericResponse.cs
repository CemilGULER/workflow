using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GenericResponse<T>
{
    public bool IsSuccess { get; set; }
    public T Result { get; set; }
    public string Message { get; set; }
}
