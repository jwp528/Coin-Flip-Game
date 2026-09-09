# CoinFlipGame.DB (retired)

SQL Server is **no longer used** for player progress or the Api runtime.

The managed Functions app (`CoinFlipGame.Api`) now stores accounts, sessions, external identity mappings, and player progress in **Azure Table Storage**. Coin catalog/images continue to use blob storage (`CoinStorageService`). This SQL project is kept in the repo only as historical schema; it is not referenced by `CoinFlipGame.sln` and is not part of the Api build.

See the root [README](../README.md) for Tables storage, Azurite, and app setting names.
