#pragma warning disable CS1591
namespace WampSharp.V2.Realm
{
    public interface IWampRealmContainer
    {
        IWampRealm GetRealmByName(string name);
    }
}