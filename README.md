# Encrypt

This is a test program that Encrypts / Decrypts any file with a user-customizable password.

BE CAREFUL!!!

This progrom does NOT store the password anywhere!!  The file is encrypted and an additional 8 bytes are appended to the end of the file
The first 4 bytes are the extension of the file (.ext .iso .exe ...)  <-- assuming the extension is 4 bytes with the '.' The next 4 bytes equals the
word "true" as a test.  If the password you entered to decrypt the file is the same as the one you used to encrypt the file, "true" will re-appear 
if decrypted correctly and the program will continue to decrypt the file and attach its original file extension.  files with no extension or shorter
extensions will encrypt, but quite possibly not able to be decrypted in the programs current state because the password check will automatically fail
since the file pointer pointing to the test word will be off if the file extension is not the full 4 bytes.

This program is a work-in-progress.  DO NOT test this file on anything you don't want to lose!
This program will attach the extension ".enc" to any file you encrypt (unless you choose otherwise)  If you open a file with the ".enc" extension,
if the password is correct, the original file WILL be overwritten without warning if you click start.

If you encrypt a file, delete the original and forget the password - Your data cannot be recovered

USE THIS PROGRAM AT YOUR OWN RISK!

