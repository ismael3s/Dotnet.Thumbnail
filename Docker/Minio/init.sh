#!/bin/sh

# Wait until MinIO is ready
until (mc alias set myminio http://localhost:9000 $MINIO_ROOT_USER $MINIO_ROOT_PASSWORD); do
  echo "Waiting for MinIO to be ready..."
  sleep 1
done

# Create a bucket
mc mb myminio/test-bucket || true

# Set public policy on the bucket (optional)
mc policy set public myminio/test-bucket
