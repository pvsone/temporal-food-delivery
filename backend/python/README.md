1. Open a terminal window in the environment and change directories to the root directory of the
`python` repository
2. Run the following command to create a virtual environment

```
$ python -m venv env
```

3. Activate the virtual environment

```
$ source env/bin/activate
```

Once the environment is active you should see `(env)` prepended to your bash prompt similar
to below

```
(env) $
```

4. Install the necessary packages into the virtual environment

```
python -m pip install -r requirements.txt
```
