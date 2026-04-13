namespace CustomPlugins.Services
{
    public interface IDuplicateContactValidator
    {
        bool EmailExists(string email);
        void ValidateNoDuplicateEmail(string email);
    }
}
