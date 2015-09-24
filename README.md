# What is this?

NestorBugs is a very simple web based bug tracking system for solo developers: it supports one single site administrator and an unlimited number of contributors. Contributors can publish bugs, the administrator can edit them and change their state.

It is built in ASP.NET MVC and uses [OpenId](http://openid.net) for user authentication. It uses [Elmah](https://code.google.com/p/elmah) for logging its own errors.

This is an old project (2011) that was originally hosted in [CodePlex](https://www.codeplex.com); you can read the original readme file (explaining the motivation for starting the project) at the end of this one. If I had to start this project from scratch today, I would have done things differently. For example I would have used [Dapper](https://github.com/StackExchange/dapper-dot-net) instead of Entity Framework, and I would have used [Bootstrap](http://getbootstrap.com/) or a similar framework for the user interface.


# How to use

You need a SQL Server database for hosting the bugs and users data. Configure the `ConnectionString` and `SiteOwnerOpenId` keys in Web.config (under `applicationSettings\Konamiman.NestorBugs.Web.Properties.Settings`) and that's it, other than that it's just a regular ASP.NET MVC site that can be hosted in IIS. By the way, the database will be created automatically if it doesn't exits.


## Testing with fake data

If you want to have a predefined set of fake bugs and users to start with, do the following:

* Set the `UseFakeData` key in Web.config to true.
* Configure the `FakeDataConnectionString` in Web.config, this is what will be used instead of `ConnectionString` if `UseFakeData` is true.
* Optionally, modify the `FakeBugsCount` and `FakeUsersCount` keys in Web.config.

Now when the application runs the database will be created if it does not exist, as usual; but additionally it will be populated with a set of fake data.


## Bypassing authentication

To automatically login users without doing all the OpenID workflow, set the `BypassOpenIdAuthentication` key in Web.config to true. Now, in the login screen you must select the "OpenID" provider type the name of one of the existing users in the database (remove the "http://" part). If you are using fake data the names of the users are "user1", "user2", etc.

When authentication is bypassed the name of the site owner is always "user1", the `SiteOwnerOpenId` key is ignored.


# Original readme

## Why another bug tracker?

I know: there are several bug trackers around, both free and paid. And probably 100% are better than what NestorBugs will get to become.

I am developing this project simply because:

* I wanted to gain some experience with web development (my programming experience is mostly with desktop application development).
* More specifically, I wanted to learn the ASP.NET MVC platform. I love the separation of concerns it provides.
* As a seconday goal, I want to get experience with Entity Framework.

And yet again, why a bug tracker? Well, I also wanted to have a simple system for tracking bugs on my MSX software. So I hope I can manage to build something minimally useful...

## Disclaimer

This is my first web application. Expect to find a lot of security, usability and performance WTFs; if you are lucky enough to find any of these, please feel free to slap me in the face by using the "Discussions" section of this site.

Currently the application is in a very early development state. There is little to see apart from the authentication system and a fake list of bugs.

My graphic design skills are close to zero, so the site is simple and ugly. If you want to contribute to the project as a designer for the HTML and CSS stuff, you are welcome.

## Wait... this reminds me of something I have seen before...

This is probably because the look & feel of NestorBugs is heavily inspired on [Stack Overflow](http://stackoverflow.com), the best programmin Q&A site around there. You know, good artists copy and great artists stole, or something... (no, I didn't stole anything from that site, that wouldn't be funny).