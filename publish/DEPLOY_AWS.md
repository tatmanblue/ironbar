# Deploying to AWS

__PLEASE NOTE__: this document is working progress

## Prerequisites

1. Create your AWS account  
2. Create ECR  
3. Create EFS
4. Create EC2
4.1 Create cluster
4.2 Create Task definition

## Manually

1. Install AWS CLI
2. aws configure  
3. aws ecr get-login-password --region us-west-2 | docker login --username {USER} --password-stdin {INSTANCE}  
4. docker push {INSTANCE}/{REGISTERY}:latest