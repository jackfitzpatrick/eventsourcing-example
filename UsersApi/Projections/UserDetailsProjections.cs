using Marten.Events.Aggregation;
using UsersApi.Events;

namespace UsersApi.Projections;

public record UserDetails(Guid Id, Guid UserId, string Name, string Email, string? Address);

public class UserDetailsProjection : SingleStreamProjection<UserDetails>
{
    public static UserDetails Create(UserRegistered evt)
    {
        return new UserDetails(evt.Id, evt.Id, evt.Name, evt.Email, null);
    }

    public UserDetails Apply(UserNameChanged evt, UserDetails currentUser) =>
        currentUser with { Name = evt.NewName };

    public UserDetails Apply(UserAddressAmended evt, UserDetails currentUser) =>
        currentUser with { Address = evt.Address };
}

