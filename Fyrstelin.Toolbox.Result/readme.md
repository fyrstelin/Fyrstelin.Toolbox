# Fyrstelin.Toolbox.Result

A monad for handling results.

## Entering the monad
Entering the monad is done with an implicit cast operator

```c#
public class MyService
{
    public Result<User, NotFound> GetUserById(string id)
    {
        var user = _users.GetOrDefault(id);
        if (user is null)
            return new NotFound(id); // implicit cast from Error 
        return user // implicit cast from Value;
    }
}
```

## Leaving the monad
Use the match operator to leave the monad
```c#
public class Controller(UserService service)
{
    public User GetUser(string id)
    {
        var userResult = service.GetUserById(id);
        //  ^ Result<User, NotFound>
        
        return userResult.Match(
            user => user,
            err => throw new NotFoundException(err.UserId)
        );
    }
}
```


## Convert
Convert is the monadic bind operator. This is the basis for all other operators.
```c#
enum Error { NotFound, InvalidName }

Result<User, NotFound> userResult = ...;

var nameResult = userResult.Convert<string, Error>(
    user => user.Name is null ? Error.InvalidName : user.Name
    err => Error.NotFound
);
//  ^ Result<string, Error>
```

If you don't need to convert the error, some extension exists:
```c#
Result<User, Error> userResult = ...;

var nameResult = userResult
    // Convert can return a Result with a new value type
    .Convert(
        user => user.Name is null ? Error.InvalidName : user.Name
    )
    // Convert can also return a value with a new type
    .Convert(x => x.Length);
//  ^ Result<int, Error>
```

### Async/await
The convert operator exists in async versions as well

## Implicit cast to boolean
An implicit cast operator exists to allow converion to boolean:
```c#
    Result<User, NotFound> result = ...;

    if (result) 
    {
        // user exists
    } else {
        // user not found
    }
```

## Combining results
You can combine results with the Result.Combine Operator:

```c#
Result<User, Error> userResult = ...;
Result<int, Error> totalUserCountResult = ...";

var results = Results.Combine(
    userResult,
    totalUserCountResult
);
//  ^ Result<(User, int), IReadOnlyCollection<Error>> 
```
If all is success, the combined result is also success with the value being a tuple. If not all errors will be listed.

## Linq
You're in for a treat! Using (or abusing) the Linq Query Comprehension syntax, you can work with results, almost as you are writing normal c#. Here are some examples of updating a user:

```c#
// Using convert
await Result.Combine(
    Email.From(request.Email),
    UserId.From(request.UserId)
).ConvertAsync(async (email, userId) => {
    var userResult = await repository.GetUserByIdAsync(userId);
    
    return userResult
        .ConvertAsync(user => user
            .UpdateEmail(email)
            .ConvertAsync(_ => repository.SaveUserAsync(userId, user));
});

// using linq
await
    from t in Result.Combine(
        Email.From(request.Email),
        UserId.From(request.UserId)
    )
    let email = t.Item1
    let userId = t.Item2
    from user in repository.GetUserByIdAsync(userId)
    where user.UpdateEmail(email)
    from result in repository.SaveUserAsync(userId, user)
    select result
```

It is up to you to 