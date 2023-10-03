# eventsourcing-example

### What is this?
- A basic event sourced system using Marten, dotnet and minimal API. Based on this guide by Oskar Dudycz https://www.youtube.com/watch?v=Lc2zV8KA16A
- Made this for a session I was running to help teach event sourcing so it's not really supposed to be the most technically sound or anywhere close to use in a real life use case, the actual domain is very bare bones and the stream design leaves a lot to be desired.
- Idea is that you can run everything locally so you can set break points, debug and mess around.

### Requirements
- dotnet 7
- Docker

### How to use it

#### Codebase overview
- Here are the key files you'll want to look at.
- `Program.cs` Here you'll find where we set up and configure Marten along with our API.
- `Services/UserCommandService.cs` In here you'll find all of our command handlers, these are triggered by requests to our API.
- `Domain/User.cs` This is our our user entity. Here is where the core of our domain logic lives.
- `Projections/UserDetailsProjections.cs` This is where we build our projections from events.
- In `Events/UserCommands.cs` you'll find all of our commands.
- In `Events/UserEvents.cs` you'll find all of our events.

#### Set up
- cd into UserApi.
- Docker compose up runs postgres and pgAdmin.
- Then run the solution. Either dotnet run on cmd line, or F5 to run in debug mode.
- Open Swagger: https://localhost:7132/swagger
- Open pgAdmin: http://localhost:5050/browser/
- In pgAdmin you can view streams, events, and read models.
- To see these tables open Servers > UserDb > Databases > postgres > Schemas > users > tables
- The tables you care about are `mt_events`, `mt_streams`, and `mt_doc_userdetails`
- The first 2 are fairly self explanatory. In events you can find all the events and the streams are in streams. `mt_doc_userdetails` is the document storage and this is where you'll read models.

#### Create a user
- Using `/api/users/register` you can create a user.
- Then in pgAdmin you can look in the events, streams and read models tables to see your newly created user.
- From the endpoint try to follow the flow through to see how the api triggers the command handler, which creates the event and then see how the projections use that event.

#### Hydrating an aggregate
- To see this in action, make changes to the user by means of the `/change-name` or `/update-address` endpoints.
- To do this you'll need the version of the stream which will increment whenever a new event is added. You can find this in `mt_streams`.
- Then you must pass this in as the If-Match header in double quotes.
- Then you'll be able to make use of these endpoints.
- Once you've made a few changes go into `Domain/User.cs` and set break points in the apply methods.
- Then try to make another change to the name or the address.
- You should see that each of the apply methods will be called for every event in the stream that they relate to, building up the entities state for the new change.

#### Fun with projections
- In pgAdmin find your user in the `mt_doc_userdetails` table.
- Once you've found your user, delete them.
- Then back in swagger use the `/api/users/rebuild-projections` endpoint.
- You should now see your user restored!
