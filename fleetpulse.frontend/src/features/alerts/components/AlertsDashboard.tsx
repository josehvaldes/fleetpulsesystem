import { Suspense, useState } from "react"
import { Header } from "@/components/layouts/header"
import { useAlerts } from "../hooks/useAlerts";


import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"

import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination"
import { AlertActionCombobox } from "./AlertRowAction";

export function AlertsDashboard() {
    const pagesize = 3;
    const [page, setPage] = useState(1);
    const { isLoading, error, alerts, totalCount, totalPages, hasNextPage, hasPreviousPage } = useAlerts(page, pagesize);

    return (
    <div>
        <Header />
        <div className="alerts-dashboard border border-blue-500 p-2">
            {isLoading && <p>Loading...</p>}
            {error && <p>Error loading alerts: {error}</p>}
            <h3 className="font-bold">Alerts Dashboard</h3>

            <Suspense fallback={<p>Loading alerts...</p>}>

            {alerts && alerts.length > 0 ? (
                <>
                <Table>
                    <TableCaption> {`Showing ${alerts.length} of ${totalCount} alerts` } </TableCaption>
                    <TableHeader>
                        <TableRow>
                            <TableHead>Alert ID</TableHead>
                            <TableHead>Driver ID</TableHead>
                            <TableHead>Event Location</TableHead>
                            <TableHead>Recommendation</TableHead>
                            <TableHead>Status</TableHead>
                            <TableHead>Quick Action</TableHead>
                            <TableHead>Show Details</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {alerts.map((alert) => (
                            <TableRow key={alert.id}>
                                <TableCell>{alert.id}</TableCell>
                                <TableCell>{alert.driverId}</TableCell>
                                <TableCell>({alert.eventLocation.latitude}, {alert.eventLocation.longitude})</TableCell>
                                <TableCell>{alert.recommendation}</TableCell>
                                <TableCell>{alert.status}</TableCell>
                                <TableCell>
                                    <AlertActionCombobox
                                        onSelectAction={(action, alertId) => {
                                            console.log(`Action "${action}" selected for alert "${alertId}"`);
                                        }}
                                        alertId={alert.id}
                                    />
                                </TableCell>
                                <TableCell>
                                    <button className="bg-blue-500 text-white px-2 py-1 rounded">View</button>
                                </TableCell>                                    
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
                <Pagination>
                    <PaginationContent>
                        <PaginationItem>
                            <PaginationPrevious
                                href="#"
                                aria-disabled={!hasPreviousPage}
                                className={!hasPreviousPage ? "pointer-events-none opacity-50" : undefined}
                                onClick={(e) => {
                                    e.preventDefault();
                                    if (hasPreviousPage) setPage((p) => p - 1);
                                }}
                            />
                        </PaginationItem>
                        <PaginationItem>
                            {Array.from({ length: totalPages }, (_, i) => i + 1).map((pageNum) => (
                                <PaginationLink key={pageNum} href="#" isActive={pageNum === page}
                                    onClick={(e) => {
                                        e.preventDefault();
                                        setPage(pageNum);
                                    }}
                                >
                                    {pageNum}
                                </PaginationLink>
                            ))}
                            
                        </PaginationItem>
                        <PaginationItem>
                            <PaginationNext
                                href="#"
                                aria-disabled={!hasNextPage}
                                className={!hasNextPage ? "pointer-events-none opacity-50" : undefined}
                                onClick={(e) => {
                                    e.preventDefault();
                                    if (hasNextPage) setPage((p) => p + 1);
                                }}
                            />
                        </PaginationItem>
                    </PaginationContent>
                </Pagination>
                </>
            ) : 
            (
                <p>No alerts found.</p>
            )}
        </Suspense>
        </div>
    </div>
    
    );
}