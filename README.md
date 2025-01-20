# File server

Very simple HTTP based file server. It uses the HTTP protocol for getting, uploading and deleting files.

The fileserver has been created to support the needs of the DAX organization. It therefore might include specialized features that the organization needs, making it not useable for other use-cases.

## Command line tools interaction with the file server

One of the main goals of the file server is to be able to interact with it using simple command line tools like `wget` and `curl`.

## Getting a file

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

## Uploading a file

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

## Creating a new directory

Example of creating a new folder named `my_newly_created_folder`. If the directory structure already exists it does nothing.

```sh
curl -u "user1:password" \
  -i -X POST \
  http://localhost:5000/my_newly_created_folder
```

Example creates both the directory named `my_first_new_folder` and then `my_second_new_folder`. If the directory structure already exists it does nothing.

```sh
curl -u "user1:password" \
  -i -X POST \
  http://localhost:5000/my_first_new_folder/my_second_new_folder
```

## Deleting a file

Showcases how to delete a file named `file.txt` in the default path.

```sh
curl -u "user1:password" \
  -i -X DELETE \
  http://localhost:5000/file.txt
```

## Deleting a directory

Example of how to delete a directory. It will recursively delete everything inside that directory.

```sh
curl -u "user1:password" \
  -i -X DELETE \
  http://localhost:5000/my_first_new_folder/my_second_new_folder
```

## Getting the contents of the folder in JSON

Example of getting the contents of the default path folder in JSON.

```sh
curl -u "user1:password" http://localhost:5000?json
```

Example of output.

```json
[
  {
    "name": "test",
    "lastWriteTimeUtc": "2025-01-20T11:42:05.1634841Z",
    "lastWriteTimeUtcUnixtimeStamp": 1737373325,
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "readme.md",
    "lastWriteTimeUtc": "2025-01-20T11:42:58.8525281Z",
    "lastWriteTimeUtcUnixtimeStamp": 1737373378,
    "fileSizeBytes": 26,
    "fileSize": "26.0 bytes",
    "isDirectory": false
  }
]
```

Example of getting the contents of a specific folder in JSON.

```sh
curl -u "user1:password" http://localhost:5000/folder_one?json
```

Example of output.

```json
[
  {
    "name": "test",
    "lastWriteTimeUtc": "2025-01-20T11:42:05.1634841Z",
    "lastWriteTimeUtcUnixtimeStamp": 1737373325,
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "readme.md",
    "lastWriteTimeUtc": "2025-01-20T11:42:58.8525281Z",
    "lastWriteTimeUtcUnixtimeStamp": 1737373378,
    "fileSizeBytes": 26,
    "fileSize": "26.0 bytes",
    "isDirectory": false
  }
]
```
