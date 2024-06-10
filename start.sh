#!/bin/bash

# Stop and remove the existing container if it exists
sudo docker-compose -f WebApplication2/docker-compose.yaml down

# Remove the previous Docker image named stripeimage3
sudo docker rmi razorimage3

# Build the Docker image in the StripePaymentDemo directory
sudo docker build -t razorimage3 -f WebApplication2/Dockerfile WebApplication2

# Bring up the Docker Compose services
sudo docker-compose -f WebApplication2/docker-compose.yaml up -d