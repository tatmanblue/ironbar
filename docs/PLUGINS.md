# Plug-ins Architecture

updated: 2025.08.04

## Overview

__Please note:__ the plugin support should be considered still very early design  

## Security

Security will be permission based.  Plug-ins will request permissions and boot noode will grant permissions.
Current thinking is the boot node will get permissions from a configuration file.  It will share those with
child nodes as needed.

Permission requests come from two technical implementations:  interfaces and enums.

Each interface implies certain behaviors.  Enums further refine those behaviors.  When a plug-in is loaded
the node will check the plug-ins requested permissions and validate they are allowed for that interface. If that 
passes, then the next pass of permission checks will occur against the configuration at the boot node.

## Behaviors
There are a couple of permitted behaviors that are defined by the interfaces.  Current thinking is
there are 2:  ledger and API.  

API plugins will only be allowed to run on the bootnode.

~~Ledger plugins will be required to run on all nodes before allowing them to work.~~  Reconsidering design to
allow ledger plugins to run on boot node only.  This would allow for more flexibility in
boot node behavior.  This is still under consideration.

## Interfaces

## Enums

