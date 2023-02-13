

using System.ComponentModel.DataAnnotations.Schema;

public class Transition : BaseDbEntity
{
    public string Name { get; set; } = string.Empty;
    
    public State FromState { get; set; } = default!;
    public State? ToState { get; set; }
   
    public ICollection<Translation> Titles { get; set; } = default!;

    public TransitionType Type { get; set; }

    public ZeebeFlow? Flow { get; set; }
    public ICollection<Translation> Forms { get; set; } = default!;
}





