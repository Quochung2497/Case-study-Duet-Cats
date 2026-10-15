namespace Game
{
    public interface INotePool
    {
        INoteBehaviour Get();
        void Release(INoteBehaviour note);
    }
}
