# Storage

This library provides implementations for various storage backends used by the application.  Ultimately,
I'd like to see these implementations be loaded through the plugin architecture.  Currently, it is statically linked 
with the rest of the project.

## Azure
The Azure types provides an implementation of the storage interfaces using Microsoft Azure as the backend.