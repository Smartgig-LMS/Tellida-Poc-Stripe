#!/bin/bash

# Stop and remove the existing container if it exists
#sudo docker-compose -f RazorPay_Payment/docker-compose.yaml down

# Remove the previous Docker image named stripeimage3
#sudo docker rmi razorpayimage

# Build the Docker image in the StripePaymentDemo directory
sudo docker build -t razorpayimage -f RazorPay_Payment/Dockerfile RazorPay_Payment

# Bring up the Docker Compose services
sudo docker-compose -f RazorPay_Payment/docker-compose.yaml up -d
