## Architecture Decision Records 0012
Testcontainer library trade-offs
Cons: 
- Slow tests execution: The initial container startup can take several seconds.
  Mitigation: Create fixture/collections to share the same container resources, and add clean and seed functions
- Docker dependency: Docker is a prerequisite when deploying the solition in a devops pipeline
  Mitigation: Azure and modern CI envrionment already support docker.
- Dependency to the database version: using a image version like ":latest" can change the results and preconditions of the tests
  Mitigation: use always a pinned version
  
Dev considerations
- Debugging the tests in Visual Studio Test Explorer was easy. break points worked as expected.
- This doesn't replace Unit testing for Domain and Application layers
- No need to use docker containers 
- Straight-forward testing: clone and run "dotnet test"
- Authentication in integration tests are handle by a custom TestAuthHandler, so no need to login or change the API