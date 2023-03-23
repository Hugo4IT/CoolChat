export class IndexRange {
    public start: number;
    public end: number;

    public constructor(start: number, end: number) {
        this.start = Math.min(start, end);
        this.end = Math.max(start, end);
    }

    public count = () => this.end - this.start;

    public contains = (index: number) =>
        (index >= this.start && index < this.end);

    public intersects = (other: IndexRange) =>
           this.contains(other.start)
        || this.contains(other.end)
        || ((other.start < this.start) == (other.end > this.end));
    
    public clamped = (range: IndexRange) =>
        new IndexRange(Math.max(this.start, range.start), Math.min(this.end, range.end));
    
    public inflated = (by: number) =>
        new IndexRange(this.start - by, this.end + by);
    
    public unsafeMerge = (other: IndexRange) => new IndexRange(Math.min(this.start, other.start), Math.max(this.end, other.end));

    public unsafeSplit = (at: number) => [
        new IndexRange(this.start, at),
        new IndexRange(at, this.end),
    ];

    public unsafeSplitByRange = (range: IndexRange) => [
        this.unsafeSplit(range.start)[0],
        this.unsafeSplit(range.end)[1],
    ];

    public split = (at: number) => {
        if (!this.contains(at))
            return undefined;
        
        return this.unsafeSplit(at);
    }

    public splitByRange = (range: IndexRange) => {
        if (!this.contains(range.start) || !this.contains(range.end))
            return undefined;
        
        return this.unsafeSplitByRange(range);
    }

    public subtract = (range: IndexRange) => {
        const hasStart = this.inflated(-1).contains(range.start);
        const hasEnd = this.inflated(-1).contains(range.end);

        if (!hasStart && !hasEnd && this.intersects(range)) {
            return []; // `range` overlaps whole `this`
        } else if (hasStart && !hasEnd) {
            return [this.unsafeSplit(range.start)[0]]; // `range` overlaps right side of `thi``
        } else if (!hasStart && hasEnd) {
            return [this.unsafeSplit(range.end)[1]]; // `range` overlaps left side of `thi``
        } else if (hasStart && hasEnd) {
            return this.unsafeSplitByRange(range);
        } else {
            return [this];
        }
    };

    public shift = (by: number) => new IndexRange(this.start + by, this.end + by);

    public static optimized = (ranges: IndexRange[]) => {
        ranges = [...ranges].sort((a, b) => a.start - b.start)
                            .filter(r => r.count() > 0);

        let index = 0;
        while (index < ranges.length - 1) {
            const cutout = ranges[index++];
            const next = ranges[index];
            
            if (cutout.inflated(1).intersects(next)) {
                const merged = cutout.unsafeMerge(next);

                // Replace old with merged and delete next
                ranges[index - 1] = merged;
                ranges.splice(index);

                index--;
            }
        }

        return ranges;
    }
}

export class IndexRangeMask {
    private base: IndexRange;
    private cutouts: IndexRange[];

    public constructor(base: IndexRange) {
        this.base = base;
        this.cutouts = [];
    }

    // Returns `this.base` with all the cutouts subtracted
    public getActiveRanges = () => {
        this.mergeCutouts();

        let active = [this.base];

        for (const cutout of this.cutouts) {
            active = active.flatMap(inverseCutout => inverseCutout.subtract(cutout));
        }

        return active;
    };

    public subtract = (range: IndexRange) => {
        if (!this.base.intersects(range))
            return;
        
        this.cutouts.push(range.clamped(this.base));
    }

    private mergeCutouts = () => {
        this.cutouts = IndexRange.optimized(this.cutouts);
    };
}

export declare type LazyLoadCallback<T> = (range: IndexRange) => PromiseLike<Array<T>>;

export class LazyLoadedArray<T> {
    private map: Map<number, T[]>;
    private loadedRanges: IndexRange[];

    private loadCallback: LazyLoadCallback<T>;

    public constructor(loadCallback: LazyLoadCallback<T>) {
        this.map = new Map();
        this.loadedRanges = [];
        this.loadCallback = loadCallback;
    }

    public pushFront = (value: T) => {
        console.log(this.map);
        if (this.map.has(0)) {
            console.log("1");
            const firstItem = [value, ...this.map.get(0)!];
            
            this.map.delete(0);
            this.shiftMap(1);
            this.map.set(0, firstItem);
            this.loadedRanges[0].start = 0;
            console.log(this.map);
        } else {
            console.log("2");
            this.shiftMap(1);
            
            this.map.set(0, [value]);
            this.loadedRanges[0] = new IndexRange(0, 1);
            console.log(this.map);
        }
        
        this.updateLoadedRanges();
        console.log(this.map);
    }

    public getRange = async (range: IndexRange) => {
        await this.ensureLoaded(range);
        return this.unsafeGetRange(range);
    }
    
    private unsafeGetRange = (range: IndexRange) => {
        const keyWithRange = this.getMapKeyContaining(range.start);
        return new Array(...this.map.get(keyWithRange)!)
                       .slice(range.start - keyWithRange, range.end - keyWithRange);
    }

    private getMapKeyContaining = (num: number) => {
        const keys: number[] = new Array(...this.map.keys());
        const key = keys.filter(k => k <= num)
                        .reduce((k, a) => Math.max(k, a), 0);
        return key;
    }

    private ensureLoaded = async (range: IndexRange) => {
        const loadMask = this.getLoadMask(range);

        if (loadMask.length <= 0)
            return;
        
        const fetched = await Promise.all(loadMask.map(this.loadCallback));

        for (let i = 0; i < fetched.length; i++) {
            const slice = loadMask[i];
            const data = fetched[i];
            
            this.map.set(slice.start, data);
            this.loadedRanges.push(slice);
        }

        this.updateLoadedRanges();
    };

    private getLoadMask = (range: IndexRange) => {
        const mask = new IndexRangeMask(range);

        for (const cutout of this.loadedRanges)
            mask.subtract(cutout);
        
        return mask.getActiveRanges();
    };

    private updateLoadedRanges = () => {
        this.loadedRanges = IndexRange.optimized(this.loadedRanges);

        // Update map to follow loadedRanges
        const newMap = new Map();
        for (const range of this.loadedRanges)
            newMap.set(range.start, this.unsafeGetRange(range));
        
        this.map = newMap;
    };

    private shiftMap = (by: number) => {
        const newMap = new Map<number, T[]>();
        for (const [key, value] of this.map.entries()) {
            newMap.set(key + by, value);
        }
        this.map = newMap;

        for (let i = 0; i < this.loadedRanges.length; i++)
            this.loadedRanges[i] = this.loadedRanges[i].shift(by);
    };

    private getTotalRange = () => new IndexRange(this.loadedRanges.at(0)?.start ?? 0, this.loadedRanges.at(-1)?.end ?? 0);
}