[![Build status](https://ci.appveyor.com/api/projects/status/lj9cm2u82kxcme58?svg=true)](https://ci.appveyor.com/project/jameshollandusa/nhibernate-blockgenerator)

nhibernate.blockgenerator
=========================

Block ID Generator for [NHibernate][A1]. Allocates IDs in blocks and tracks the next available ID
in the database. Allows the size of blocks to vary on each allocation. This is useful for the following
reasons:

* allows variation between clients e.g. a bulk insert tool can allocate a block of 1000 and web
clients can allocate a block of 10
* allows the block size to be different between entities in the same client
* allows the block size to be changed at any time

It is conceptually similar to the hilo algorithm in that it allocates chunks of ids and tracks
what has been allocated in a database table. The difference is that the hilo algorithm requires
each chunk to be a fixed size that is defined once and never changed, or at least
not without significant coordination between all clients. This is because the hilo algorithm
only tracks the hi value in the database. This hi value must then be multipled by the max_lo stored
in each client to determine the next available ID. 

Instead this algorithm stores the next available ID in the database and increments it by the
requested block size on each allocation. This allows each allocation to have a different block size.

[A1]: http://www.nhforge.org

Licenses
--------

This software is distributed under the terms of the Free Software Foundation [Lesser GNU Public License (LGPL), version 2.1][B1] (see lgpl.txt).

[B1]: http://www.gnu.org/licenses/lgpl-2.1-standalone.html
