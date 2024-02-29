# Food Delivery App

This repository contains alternate backend implementations for the [Food Delivery App](https://github.com/temporalio/samples-typescript/tree/main/food-delivery).

The original Food Delivery sample is fully implemented in TypeScript, both the frontend and the backend.  It is a wonderful example of a full stack application using the Temporal TypeScript SDK.  If you have not explored that example, please do so along with the excellent companion blog posts:
* [Building Reliable Distributed Systems in Node.js](https://temporal.io/blog/building-reliable-distributed-systems-in-node)
* [How Durable Execution Works](https://temporal.io/blog/building-reliable-distributed-systems-in-node-js-part-2)

## Frontend
The frontend for this sample is the same as the original Food Delivery App. The code has been copied into this repository for convenience, with minimal changes to support the alternate backend implementations.

To run the frontend, see the instructions in [frontend/README.md](frontend/README.md).

## Backend
This repository contains multiple alternate backends for the Food Delivery App.  Each backend is implemented in a different language and uses the Temporal SDK for that language.

You will only need to run one of the backends at a time. To pick and run a backend, see the instructions [backend/README.md](backend/README.md).

## Temporal
This sample will work with either the [Temporal dev server](https://docs.temporal.io/cli#start-dev-server) or [Temporal Cloud](https://docs.temporal.io/cloud).

### Temporal Dev Server
If you are using the Temporal dev server, simply execute the following command to start the server:
```bash
temporal server start-dev
```

You can access the Temporal Web UI at http://localhost:8233.

### Temporal Cloud
If using Temporal Cloud, please set the following environment variables:
```bash
export TEMPORAL_ADDRESS=<your_namespace>.<your_account>.tmprl.cloud:7233
export TEMPORAL_NAMESPACE=<your_namespace>.<your_account>
export TEMPORAL_TLS_CERT=/path/to/client.crt
export TEMPORAL_TLS_KEY=/path/to/client.key
export TEMPORAL_TLS_KEY_PKCS8=/path/to/pkcs8/client.key
```

*The `TEMPORAL_TLS_KEY_PKCS8` environment variable is __only__ required if you are using the __Java__ backend.*

You can access the Temporal Web UI at https://cloud.temporal.io
