#!/bin/bash

# Stop and remove the existing container if it exists
sudo docker-compose -f StripePaymentDemo/docker-compose.yaml down

# Remove the previous Docker image named stripeimage3
sudo docker rmi stripeimage3

# Build the Docker image in the StripePaymentDemo directory
sudo docker build -t stripeimage3 -f StripePaymentDemo/Dockerfile StripePaymentDemo

# Bring up the Docker Compose services
sudo docker-compose -f StripePaymentDemo/docker-compose.yaml up -d