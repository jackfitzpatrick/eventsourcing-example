namespace UsersApi.Events
{
    public record UserRegistered(
        Guid Id,
        string Name,
        string Email
        );

    public record UserNameChanged(Guid Id, string NewName);

    public record UserAddressAmended(Guid Id, string Address);
}
