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

Showcases how to delet a file named `file.txt` in the default path.

```sh
curl -u "user1:password" \
  -i -X DELETE \
  http://localhost:5000/file.txt
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
    "name": "folder_one",
    "lastWriteTimeUtc": "2025-01-08T15:09:44.8965425Z",
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "folder_three",
    "lastWriteTimeUtc": "2025-01-07T09:25:14.7323732Z",
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "folder_two",
    "lastWriteTimeUtc": "2025-01-10T10:29:41.9617931Z",
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "my_text.txt",
    "lastWriteTimeUtc": "2025-01-10T10:27:30.4823977Z",
    "fileSizeBytes": 12894,
    "fileSize": "12.6 KB",
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
    "name": "documents",
    "lastWriteTimeUtc": "2025-01-08T16:12:19.3597558Z",
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "images",
    "lastWriteTimeUtc": "2025-01-08T16:12:55.2529702Z",
    "fileSizeBytes": null,
    "fileSize": null,
    "isDirectory": true
  },
  {
    "name": "20241104_134204.jpg",
    "lastWriteTimeUtc": "2025-01-08T13:26:21.6989601Z",
    "fileSizeBytes": 7993054,
    "fileSize": "7.6 MB",
    "isDirectory": false
  },
  {
    "name": "my_file_x.xml",
    "lastWriteTimeUtc": "2025-01-08T13:04:10.6502739Z",
    "fileSizeBytes": 557,
    "fileSize": "557.0 bytes",
    "isDirectory": false
  },
  {
    "name": "my_text.txt",
    "lastWriteTimeUtc": "2025-01-08T15:09:44.8965425Z",
    "fileSizeBytes": 12894,
    "fileSize": "12.6 KB",
    "isDirectory": false
  }
]
```
