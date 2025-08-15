# Core

## Interfaces
The original design, many moons ago (I was really surprised to realize this project has been around for more than 6 years now), 
included a couple of concepts unique to most block chain implementations.  

One of those decisions was to allow plugins to extend the core functionality of the application.  An example might be
adding support for a credential which would not be part of the core application.   To ensure plugins behaved well, plugins 
would not interact directly with concrete implementations (other than those created in the plugin), but rather 
with interfaces defined in the core.

Another design choice was to allow nodes to handle different ledgers at the same time (again through plugins).
This helped drive the dessign decision that the core had to define a lot of abstractions.
 
Because of these design considerations, the core defines a lot interfaces that do not seem necessary right now.  
  

