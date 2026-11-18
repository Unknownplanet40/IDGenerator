namespace IdGen.Core.Exceptions;

public class SectionInUseException : InvalidOperationException
{
    public SectionInUseException(string message) : base(message)
    {
    }

    public SectionInUseException(int sectionId) 
        : base($"Cannot delete section ID {sectionId} because it is referenced by one or more student ID submissions.")
    {
    }
}
