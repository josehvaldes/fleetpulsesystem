## Architecture Decision Records 0009

** Add '@tanstack/react-query'**
needed for caching and alerts mutation. 
Better than fetching and calling the API directly.

 ** Exclude @tanstack/react-query-persist-client and @tanstack/query-async-storage-persister**
Contratry to previous e-commerce projects, the FleetPulse doesn't need to cache the alerts or drivers information. 
Offline support is out of the scope. there are not large datasets to query