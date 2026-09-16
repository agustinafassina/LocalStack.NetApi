#!/bin/bash
awslocal s3 mb s3://local-bucket 2>/dev/null || true
