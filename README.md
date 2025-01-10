# File server

## Introduction

Very simple HTTP based file server. It uses the HTTP protocol for getting, uploading and deleting files.

The fileserver has been created to support the needs of the DAX organization. It therefore might include specialized features that the organization needs, making it not useable for other us-cases.

## Command line tools interaction with the file server

One of the main goals of the file server is to be able to interact with it using simple command line tools like `wget`.

### Getting a file

```
wget --user user1 --password password http://localhost:5000/file.txt
```
