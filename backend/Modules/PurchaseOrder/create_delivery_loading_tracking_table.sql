-- PostgreSQL Script to create DeliveryLoadingTracking table

CREATE TABLE IF NOT EXISTS public.delivery_loading_tracking
(
    "UID" uuid NOT NULL DEFAULT gen_random_uuid(),
    "PurchaseOrderUID" uuid NOT NULL,
    "VehicleUID" uuid,
    "DriverEmployeeUID" uuid,
    "ForkLiftOperatorUID" uuid,
    "SecurityOfficerUID" uuid,
    "ArrivalTime" timestamp without time zone,
    "LoadingStartTime" timestamp without time zone,
    "LoadingEndTime" timestamp without time zone,
    "DepartureTime" timestamp without time zone,
    "LogisticsSignature" text,
    "DriverSignature" text,
    "Notes" text,
    "IsActive" boolean NOT NULL DEFAULT true,
    "CreatedDate" timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" uuid,
    "ModifiedDate" timestamp without time zone,
    "ModifiedBy" uuid,
    CONSTRAINT "DeliveryLoadingTracking_pkey" PRIMARY KEY ("UID"),
    CONSTRAINT "FK_DeliveryLoadingTracking_PurchaseOrder" FOREIGN KEY ("PurchaseOrderUID")
        REFERENCES public."PurchaseOrderHeader" ("UID") MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS "IX_DeliveryLoadingTracking_PurchaseOrderUID"
    ON public."DeliveryLoadingTracking" ("PurchaseOrderUID");

CREATE INDEX IF NOT EXISTS "IX_DeliveryLoadingTracking_VehicleUID"
    ON public."DeliveryLoadingTracking" ("VehicleUID");

CREATE INDEX IF NOT EXISTS "IX_DeliveryLoadingTracking_CreatedDate"
    ON public."DeliveryLoadingTracking" ("CreatedDate" DESC);

COMMENT ON TABLE public."DeliveryLoadingTracking"
    IS 'Tracks delivery loading information including vehicle, personnel, timings, and signatures for Step 1.1.7 of delivery process';
