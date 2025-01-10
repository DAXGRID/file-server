# File server

## Introduction

Very simple HTTP based file server. It uses the HTTP protocol for getting, uploading and deleting files.

The fileserver has been created to support the needs of the DAX organization. It therefore might include specialized features that the organization needs, making it not useable for other us-cases.

## Command line tools interaction with the file server

One of the main goals of the file server is to be able to interact with it using simple command line tools like `wget` and `curl`.

### Getting a file

Example using `curl`.

```
curl -o "file.txt" -u "user1:password" http://localhost:5000/file.txt
```

Example using `curl` with a capture group so the file name does not have to specified twice.

```
curl -o "#1" -u "user1:password" http://localhost:5000/{file.txt}
```

Example using `wget`.

```
wget --user user1 --password password http://localhost:5000/file.txt
```

### Uploading a file

Example uploading a file with `curl` in the default path.

```sh
curl -u "user1:password" \
  -i -X POST -H "Content-Type: multipart/form-data" \
  -F "data=@my_text.txt" \
  http://localhost:5000
```

Example uploading a file with `curl` in another path that is not the default.

```sh
curl -u "user1:password" \
  -i -X POST -H "Content-Type: multipart/form-data" \
  -F "data=@my_text.txt" \
  http://localhost:5000/folder_two
```
