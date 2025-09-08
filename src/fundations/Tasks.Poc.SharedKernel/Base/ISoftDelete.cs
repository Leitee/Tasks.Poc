namespace Tasks.Poc.SharedKernel.Base;

public interface ISoftDelete
{
    bool IsDeleted { get; }

    void Delete();
}
