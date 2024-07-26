# Python Backend

## Running the Python backend 

1. Run the following command to create a virtual environment

```
$ python -m venv env
```

2. Activate the virtual environment

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

5. Run the worker

```
python worker.py
```
