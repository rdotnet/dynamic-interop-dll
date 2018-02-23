# Contexts

* You have an application where the front end is in .NET, you call a function in the native library that leads to a long running task. You want to have this long running task report on its progress to provide a progress update to users.
* You want to be able to report unexpected messages (e.g. exceptions) emanating from the native library back to the .NET front end of your application.

# Possible solutions

* callback (needs a reference: wikipedia?)
* polling (needs a reference: wikipedia?)
* other?

# Callback

