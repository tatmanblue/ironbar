# Revocable Credentials

## Terms
Consumer - a general person, typically the holder of a credential   
Prover - specific consumer that owns schemas and can prove a credential is valid.  
User - a specific consumer that requests proof a credential meets their criteria.  
Credential - a set if data that can be used to prove (or disprove) a consumer meets specific criteria.  


## References
[Revocable Blockcahin](https://tykn.tech/identity-revocation-blockchain/)

## API
These are supported APIS

### CreateSchema
Schema defines how a credential looks.  It is required because a credential will be built on a specific schema.  

### CreateCredential
This is called by a consumer.  Credential is the data matching a schema that can be used to determine
the identity of the consumer.  It is tied to consumer (who is required to store in their wallet keys and data) 
and is owned by the consumer.  In the process of creating the credential, a prover must prove the credential is true.

### ProveCredential
This is called by a prover who can verify a credential as true.  Typically, the prover created the schema and then
invited a consumer.  But it is not a requirement that the prover owns the schema. 

### RevokeCredential

## Later APIS

### UpdateSchema

### UpdateCredential
