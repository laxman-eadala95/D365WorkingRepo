using System;

namespace CustomPlugins.Services
{
    public interface IChildContactService
    {
        Guid CreateChildContact(Guid accountId, string accountName);
    }
}
