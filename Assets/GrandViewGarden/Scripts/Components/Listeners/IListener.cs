using System.Collections.Generic;

public interface IListener<T>
{
    List<T> Targets { get; set; }
}
