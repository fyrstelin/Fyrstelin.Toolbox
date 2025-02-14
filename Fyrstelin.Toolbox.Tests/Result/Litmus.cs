using Fyrstelin.Toolbox.Linq;

namespace Fyrstelin.Toolbox.Tests.Results;

public class Litmus
{
    [Fact]
    public async Task Test()
    {
        var request = 
        (
            Email: "",
            UserId: ""
        );
        var repository = new Repository();

        var res = await (
            from tuple in Result.Combine(
                Email.From(request.Email),
                UserId.From(request.UserId)
            ).Convert((email, userId) => (email, userId))
            from user in repository.GetUserByIdAsync(tuple.userId)
            where user.UpdateEmail(tuple.email)
            from x in repository.SaveUserAsync(tuple.userId, user)
            select x
        );
        
    }

    public class Repository
    {
        public Task<Result<User, Error>> GetUserByIdAsync(UserId userId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<object,Error>> SaveUserAsync(UserId userId, User user)
        {
            throw new NotImplementedException();
        }
    }

    public record User
    {
        public Result<object,Error> UpdateEmail(Email email)
        {
            throw new NotImplementedException();
        }
    }
    
    public class Email
    {
        public static Result<Email, Error> From(string email) => default!;
    }
    public class UserId
    {
        public static Result<UserId, Error> From(string id) => default!;
    }

    public enum Error
    {
        
    }
}